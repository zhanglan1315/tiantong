using Microsoft.EntityFrameworkCore;
using Midos.Domain;
using Namei.Wcs.Database;
using Namei.Wcs.Aggregates;

namespace Namei.Wcs.Api
{
  public class DomainContext: Midos.Domain.DomainContext
  {
    public DbSet<Job> Jobs { get; set; }

    public DbSet<Log> Logs { get; set; }

    public DbSet<Device> Devices { get; set; }

    public DbSet<RcsAgcTask> RcsAgcTasks { get; set; }

    public DbSet<RcsDoorTask> RcsDoorTasks { get; set; }

    public DbSet<DeviceError> DeviceErrors { get; set; }

    public DbSet<DeviceState> DeviceStates { get; set; }

    public DbSet<LifterTask> LifterTasks { get; set; }

    public DbSet<WcsDoorPassport> WcsDoorPassports { get; set; }

    public DomainContext(IDomainContextOptions<DomainContext> options, IEventPublisher publisher)
      : base(options, publisher)
    {

    }
  }
}
