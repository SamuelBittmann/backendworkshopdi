using BullshitApi.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BullshitApi.Data
{
    public class InMemoryBullshitRepository
    {
        private static InMemoryBullshitRepository instance;

        public static InMemoryBullshitRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InMemoryBullshitRepository();
                }

                return instance;
            }
        }

        private InMemoryBullshitRepository() { }

        private readonly IList<BullshitModel> bullshits = new List<BullshitModel>();

        public IList<BullshitModel> GetAll()
        {
            return bullshits;
        }

        public void Add(BullshitModel bullshit)
        {
            this.bullshits.Add(bullshit);
        }
    }
}
