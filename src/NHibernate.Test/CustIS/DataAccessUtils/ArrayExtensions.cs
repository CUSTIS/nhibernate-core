namespace System
{
    public static class ArrayExtensions
    {
        public static void Fill(this Array arrayToFill, object fillValue)
        {
            // If called with a single value, wrap the value in an array and call the main function
            Fill(arrayToFill, new [] { fillValue });
        }

        public static void Fill(this Array arrayToFill, Array fillValue)
        {
            if (fillValue.Length >= arrayToFill.Length)
            {
                throw new ArgumentException("fillValue array length must be smaller than length of arrayToFill");
            }

            // set the initial array value
            Array.Copy(fillValue, arrayToFill, fillValue.Length);

            int arrayToFillHalfLength = arrayToFill.Length / 2;

            for (int i = fillValue.Length; i < arrayToFill.Length; i *= 2)
            {
                int copyLength = i;
                if (i > arrayToFillHalfLength)
                {
                    copyLength = arrayToFill.Length - i;
                }

                Array.Copy(arrayToFill, 0, arrayToFill, i, copyLength);
            }
        }
    }
}
