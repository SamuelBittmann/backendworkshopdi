using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BullshitApi.Business
{
    public class BullshitModel
    {
        public readonly string Bullshitter;
        public readonly string Text;

        public BullshitModel(string bullshitter, string text)
        {
            Bullshitter = bullshitter;
            Text = text;
        }
    }
}
