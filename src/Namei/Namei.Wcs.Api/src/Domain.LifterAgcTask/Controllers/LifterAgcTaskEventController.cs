// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Midos.Eventing;
// using Midos.Services.Http;
// using Namei.Wcs.Api;
// using System.Linq;
// using System.Text.Json;

// namespace Namei.Wcs.Aggregates
// {
//   public class LifterAgcTaskEventController: BaseController
//   {
//     public const string Group = "LifterAgcTaskEventController";

//     private WcsContext _context;

//     public LifterAgcTaskEventController(WcsContext context)
//     {
//       _context = context;
//     }

//     [EventSubscribe(LifterAgcTaskEvent.Created, Group)]
//     public void Created(LifterAgcTaskEvent param)
//     {
//       _context.Publish(LifterAgcTaskEvent.Started, param);
//     }

//     [EventSubscribe(LifterAgcTaskEvent.Started, Group)]
//     public void Started(LifterAgcTaskEvent param)
//     {
//       var task = _context.Set<LifterAgcTask>()
//         .Include(task => task.Type)
//         .FirstOrDefault(task => task.Id == param.Id);

//       if (task == null) {
//         return;
//       }

//       task.Export();
//       _context.SaveChanges();
//       _context.Publish(LifterAgcTaskEvent.Exported, param);
//     }

//     [EventSubscribe(LifterAgcTaskEvent.Exported, Group)]
//     public void Export(LifterAgcTaskEvent param)
//     {
//       var task = _context.Set<LifterAgcTask>()
//         .Include(task => task.Type)
//         .FirstOrDefault(task => task.Id == param.Id);

//       if (task == null) {
//         return;
//       }

//       _context.Publish(RcsAgcTaskCreate.Message, RcsAgcTaskCreate.From(
//         taskType: RcsAgcTaskMethod.Carry,
//         position: task.Position,
//         destination: task.Destination,
//         priority: "",
//         podCode: task.PalletCode,
//         comment: "",
//         orderType: "export",
//         orderId: task.Id
//       ));
//     }

//     [RcsAgcTaskOrderFinished("export", Group = Group)]
//     public void Finish(RcsAgcTaskOrderFinished param)
//     {
//       var task = _context.Find<LifterAgcTask>(param.OrderId);

//       if (task == null) {
//         return;
//       }

//       task.Finish();
//       _context.SaveChanges();
//       _context.Publish(
//         LifterAgcTaskEvent.Finished,
//         LifterAgcTaskEvent.From(param.OrderId)
//       );
//     }

//     [EventSubscribe(LifterAgcTaskEvent.Finished, Group)]
//     public void Finished(LifterAgcTaskEvent param)
//     {
//       var task = _context.Set<LifterAgcTask>()
//         .Include(task => task.Type)
//         .FirstOrDefault(task => task.Id == param.Id);

//       if (task == null) {
//         return;
//       }

//       _context.Publish(
//         HttpPost.Event,
//         HttpPost.From(
//           url: task.Type.WebHook,
//           data: JsonSerializer.Serialize(new { OrderId = task.OrderId })
//         )
//       );
//     }
//   }
// }
