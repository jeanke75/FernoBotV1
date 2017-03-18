using System;

namespace FernoBotV1.Services.Exceptions
{
    public class RPGException : Exception
    {
        public RPGException() : base() { }
        public RPGException(string message) : base(message) { }
    }

    public class RPGItemNotFoundException : RPGException
    {
        public RPGItemNotFoundException(int itemId) : base($"no item could be found with ID = {itemId}.") { }
        public RPGItemNotFoundException(string itemName) : base($"no item could be found thats goes by the name of (or contains) '{itemName}'.") { }
    }

    public class RPGUserNotFoundException : RPGException
    {
        public RPGUserNotFoundException() : base() { }
    }

    public class RPGInvalidItemTypeException : RPGException
    {
        public RPGInvalidItemTypeException(string message) : base(message) { }
    }
}
