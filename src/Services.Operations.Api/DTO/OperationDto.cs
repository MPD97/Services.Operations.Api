﻿using Thesis.Services.Operations.Api.Types;

namespace Thesis.Services.Operations.Api.DTO
{
    public class OperationDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public OperationState State { get; set; }
        public string Code { get; set; }
        public string Reason { get; set; }
        
        public dynamic Data { get; set; }
    }
}