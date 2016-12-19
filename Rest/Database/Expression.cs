using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Database
{

    public class Expression
    {
        private string expr;

        public Expression(string expr, bool qoute = true)
        {
            if (qoute)
                this.expr = "'" + expr + "'";
            else
                this.expr = expr;
        }

        public override string ToString()
        {
            return expr;
        }
    }
}
