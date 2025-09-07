
using Microsoft.EntityFrameworkCore;
using scommon.Auths;
using scommon.DbSettings;

namespace scommon_sample_web_api.Contexts;

public class SampleContext : BaseDbContext<SampleContext>
{
    public SampleContext(IServiceProvider serviceProvider, DbContextOptions<SampleContext> option, ICurrentUser currentUser) : base(serviceProvider, option, currentUser)
    {
    }
}
