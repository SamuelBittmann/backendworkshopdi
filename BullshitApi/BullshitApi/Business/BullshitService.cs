using BullshitApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BullshitApi.Business
{
    public class BullshitService
    {
        public IList<BullshitModel> GetAll()
        {
            return InMemoryBullshitRepository.Instance.GetAll();
        }

        public void Add(BullshitModel bullshit)
        {
            var bullshitters = InMemoryBullshitterRepository.Instance;
            if (!bullshitters.Exists(bullshit.Bullshitter))
            {
                bullshitters.Add(bullshit.Bullshitter);
            }

            InMemoryBullshitRepository.Instance.Add(bullshit);
        }

        public ISet<string> GetBullshitters()
        {
            return InMemoryBullshitterRepository.Instance.GetAll();
        }
    }
}
