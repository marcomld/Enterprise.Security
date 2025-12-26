using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Common
{
    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(bool success, T? value, string? error)
            : base(success, error)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, null);
        public static Result<T> Failure(string error) => new(false, default, error);
    }


    // Sugerencia: Clase Result simple para operaciones void (Logout, Audit)
    public class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }

        protected Result(bool success, string? error) // Cambiado de 'private' a 'protected'
        {
            IsSuccess = success;
            Error = error;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(string error) => new(false, error);
    }
}
