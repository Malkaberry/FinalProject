using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Programmin2_classroom.Shared.Models.present.toAdd;
using Programmin2_classroom.Shared.Models.present.toShow;
using Programmin2_classroom.Shared.Models.present.toEdit;
using TriangleDbRepository;

namespace Programmin2_classroom.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly DbRepository _db;

        public TransactionsController(DbRepository db)
        {
            _db = db;
        }

        
    }
}
