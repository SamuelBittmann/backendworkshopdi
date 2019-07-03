using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BullshitApi.Data
{
    public class InMemoryBullshitterRepository
    {
        private static InMemoryBullshitterRepository instance;

        public static InMemoryBullshitterRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InMemoryBullshitterRepository();
                }

                return instance;
            }
        }

        private readonly ISet<string> bullshitter = new HashSet<string>();

        private InMemoryBullshitterRepository() { }

        public ISet<string> GetAll()
        {
            return bullshitter;
        }

        public bool Exists(string bullshitter)
        {
            return this.bullshitter.Contains(bullshitter);
        }

        public void Add(string bullshitter)
        {
            this.bullshitter.Add(bullshitter);
        }
    }
}
