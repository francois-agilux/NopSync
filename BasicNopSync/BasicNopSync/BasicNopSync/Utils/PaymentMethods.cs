using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Utils
{
    class PaymentMethods
    {        
        public const string M_VIREMENT = "9";
        public const string M_PAYPAL = "10";
    
        
        public const string NOP_VIREMENT = "Payments.CheckMoneyOrder";
        public const string NOP_PAYPAL = "Payments.PayPalStandard";
    }
}
