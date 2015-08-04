using System;
using System.Threading.Tasks;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IJobService
    {
        Task<string> GetJob(DateTime month);
    }
}