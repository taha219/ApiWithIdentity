﻿namespace Identity_Roles_API.Data
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }
        public T? Data { get; set; }
    }
}
