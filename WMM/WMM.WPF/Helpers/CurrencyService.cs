using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMM.WPF.Helpers
{
    public class CurrencyService
    {
        private struct Currency
        {
            public string Code;
            public double Rate; // rate against the system default currency
        }

        private List<Currency> _currencies;

        public CurrencyService(string currencySettingsString)
        {
            _currencies = new List<Currency> { new Currency { Code = Default, Rate = 1.0 } };

            try
            {
                var currencyStrings = currencySettingsString.Split(';');

                for (int i = 0; i < currencyStrings.Length; i++)
                {
                    var code = currencyStrings[i].Split(':')[0];
                    var rate = double.Parse(currencyStrings[i].Split(':')[1], CultureInfo.InvariantCulture);
                    _currencies.Add(new Currency { Code = code, Rate = rate });
                }
            }
            catch (Exception e)
            {
                
            }
        }

        public string Default => "EUR";

        public bool HasMultipleCurrencies => Currencies.Count > 1;

        public List<string> Currencies
        {
            get { return _currencies.Select(x => x.Code).ToList(); }
        }

        public double Convert(string from, double value)
        {
            var currency = _currencies.First(x => x.Code == from);
            return value * currency.Rate;
        }



        
        
    }
}
