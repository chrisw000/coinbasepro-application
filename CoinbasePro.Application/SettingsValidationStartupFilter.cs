using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace CoinbasePro.Application
{
    // From: https://andrewlock.net/adding-validation-to-strongly-typed-configuration-objects-in-asp-net-core/
    public interface IValidateStartUp
    {
        void Validate();
    }

    public class SettingValidationException : Exception
    {
        public string ErrorValidatingType { get; }
        public string ErrorValidatingProperty { get; }
        
        public SettingValidationException(string type, string property, string message) : base(message)
        {
            ErrorValidatingType = type;
            ErrorValidatingProperty = property;
        }
    }

    public class SettingValidationStartupFilter : IStartupFilter
    {
        readonly IEnumerable<IValidateStartUp> _validate;
        public SettingValidationStartupFilter(IEnumerable<IValidateStartUp> validate)
        {
            _validate = validate;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            /*
             * This IStartupFilter doesn't modify the middleware pipeline: it returns next without modifying it.
             * But if any IValidateStartUp throw an exception, then the exception will bubble up, and prevent the app from starting.
             */
            foreach (var obj in _validate)
            {
                obj.Validate();
            }

            //don't alter the configuration
            return next;
        }
    }
}
