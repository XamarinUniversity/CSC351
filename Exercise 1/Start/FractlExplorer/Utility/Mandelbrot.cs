using System.Numerics;

namespace FractlExplorer
{
    public class Mandelbrot
    {
        public void RenderRow(double real, double imaginary, double realIncrement, 
                int maxIterations, int[] palette, int[] pixels, int startIndex, int length)
        {
            for (int x = 0; x < length; x++)
            {
                Complex z = new Complex(0, 0);
                Complex c = new Complex(real + x * realIncrement, imaginary);

                int reps = 1;

                // Work until the magnitude squared > 4.
                do
                {
                    z = z.SquareAndAdd(c);
                    reps++;
                } 
                while (reps <= maxIterations && z.GetSquaredModulus() < 4.0f);

                pixels[x+startIndex] = reps < maxIterations 
                    ? palette[(reps % palette.Length)] 
                    : palette[0];
            }
        }
    }
}
