// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.HttpRepl
{
    public class TryResult<T>
    {
        private static readonly TryResult<T> FailedResult = new TryResult<T>(false, default);

        public bool Success { get; }
        public T Output { get; }

        private TryResult(bool success, T output)
        {
            Success = success;
            Output = output;
        }

        public static TryResult<T> Succeeded(T output)
        {
            return new TryResult<T>(true, output);
        }

        public static TryResult<T> Failed()
        {
            return FailedResult;
        }
    }
}
