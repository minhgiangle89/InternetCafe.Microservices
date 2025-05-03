using InternetCafe.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthUserService.Domain.Exceptions
{
    public class DuplicateUserException : DomainException
    {
        public DuplicateUserException(string username)
            : base($"User with username '{username}' already exists.")
        {
            Username = username;
        }

        public DuplicateUserException(string fieldName, string value)
            : base($"User with {fieldName} '{value}' already exists.")
        {
            FieldName = fieldName;
            Value = value;
        }

        public string? Username { get; }
        public string? FieldName { get; }
        public string? Value { get; }
    }
}
