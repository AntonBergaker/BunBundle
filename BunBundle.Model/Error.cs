using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    public class Error {
        public readonly Exception? Exception;
        public readonly string ErrorMessage;

        public Error(Exception exception) {
            this.Exception = exception;
            this.ErrorMessage = exception.ToString();
        }

        public Error(string message) {
            this.ErrorMessage = message;
        }
    }
}
