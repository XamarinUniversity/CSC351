namespace System.Numerics
{
    public static class ComplexExtensions
    {
        // This is a shortcut for z->z^2+c
        public static Complex SquareAndAdd(this Complex me, Complex c)
        {
            //(a + bi)(a + bi) + (c+di) =(a^2 - b^2 + c) + (2ab + d)i;
            double r = (me.Real * me.Real) - (me.Imaginary * me.Imaginary) + c.Real;
            double i = (2.0 * me.Real * me.Imaginary) + c.Imaginary;
            return new Complex(r,i);
        }

        public static double GetSquaredModulus(this Complex me)
        {
            double x = me.Real;
            double y = me.Imaginary;
            return (x * x + y * y);
        }
    }
}

