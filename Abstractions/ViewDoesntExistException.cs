using System;

namespace Abstractions
{
    public sealed class ViewDoesntExistException : Exception
    {
        public ViewDoesntExistException(IViewId viewId)
            : base($"View doesn't exists for view filter: '{viewId}'.")
        {
        }
    }
}