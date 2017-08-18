using System;

namespace Hake.Extension.DependencyInjection.Abstraction
{
    public sealed class ArgumentTraverseContext
    {
        public object[] Arguments { get; }

        private int searchStart;
        private int searchEnd;
        private int argumentCount;
        private int cursorIndex;
        private bool[] isArgumentUsed;

        internal ArgumentTraverseContext(object[] arguments)
        {
            Arguments = arguments;
            searchStart = 0;
            argumentCount = arguments.Length;
            searchEnd = argumentCount - 1;
            cursorIndex = searchStart;
            isArgumentUsed = new bool[argumentCount];
        }

        public void Reset()
        {
            cursorIndex = searchStart;
        }
        public bool GoNext(Func<object, int, bool> action)
        {
            while (cursorIndex <= searchEnd && isArgumentUsed[cursorIndex]) cursorIndex++;
            if (cursorIndex > searchEnd)
                return false;

            bool result = action(Arguments[cursorIndex], cursorIndex);
            if (result)
            {
                isArgumentUsed[cursorIndex] = true;
                if (cursorIndex == searchStart)
                    searchStart++;
                while (searchStart < argumentCount && isArgumentUsed[searchStart])
                    searchStart++;
                if (cursorIndex == searchEnd)
                    searchEnd--;
                while (searchEnd >= 0 && isArgumentUsed[searchEnd])
                    searchEnd--;
            }
            cursorIndex++;
            return true;
        }
    }
}
