﻿using System;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;
using Microsoft.Extensions.Logging;
using Thesis.Services.Operations.Api.Infrastructure;
using Thesis.Services.Operations.Api.Services;
using Thesis.Services.Operations.Api.Types;

namespace Thesis.Services.Operations.Api.Handlers
{
    public class GenericCommandHandler<T> : ICommandHandler<T> where T : class, ICommand
    {
        private readonly ICorrelationContextAccessor _contextAccessor;
        private readonly IMessagePropertiesAccessor _messagePropertiesAccessor;
        private readonly IOperationsService _operationsService;
        private readonly IHubService _hubService;
        private readonly ILogger<GenericCommandHandler<T>> _logger;

        public GenericCommandHandler(ICorrelationContextAccessor contextAccessor,
            IMessagePropertiesAccessor messagePropertiesAccessor,
            IOperationsService operationsService, IHubService hubService, 
            ILogger<GenericCommandHandler<T>> logger)
        {
            _contextAccessor = contextAccessor;
            _messagePropertiesAccessor = messagePropertiesAccessor;
            _operationsService = operationsService;
            _hubService = hubService;
            _logger = logger;
        }

        public async Task HandleAsync(T command)
        {
            var messageProperties = _messagePropertiesAccessor.MessageProperties;
            var correlationId = messageProperties?.CorrelationId;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                return;
            }

            var context = _contextAccessor.GetCorrelationContext();
            var name = string.IsNullOrWhiteSpace(context?.Name) ? command.GetType().Name : context.Name;
            var userId = context?.User?.Id;
            var state = messageProperties.GetSagaState() ?? OperationState.Pending;
            var (updated, operation) = await _operationsService.TrySetAsync(correlationId, userId, name, state);
            if (!updated)
            {
                return;
            }
            
            switch (state)
            {
                case OperationState.Pending:
                    await _hubService.PublishOperationPendingAsync(operation);
                    break;
                case OperationState.Completed:
                    await _hubService.PublishOperationCompletedAsync(operation);
                    break;
                case OperationState.Rejected:
                    await _hubService.PublishOperationRejectedAsync(operation);
                    break;
                default:
                    throw new ArgumentException($"Invalid operation state: {state}", nameof(state));
            }
        }
    }
}