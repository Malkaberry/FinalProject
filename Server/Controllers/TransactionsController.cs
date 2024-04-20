using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Programmin2_classroom.Shared.Models.present.toAdd;
using Programmin2_classroom.Shared.Models.present.toShow;
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

        [HttpPost("AddTransaction")] // יצירת הזנה חדשה
        public async Task<IActionResult> AddTransaction(TransactionToAdd TransactionToAdd)
        {
            object TransToAddParam = new
            {
                transTitle= TransactionToAdd.transTitle,
                subCategoryID= TransactionToAdd.subCategoryID,
                transType= TransactionToAdd.transType,
                transValue= TransactionToAdd.transValue,
                valueType= TransactionToAdd.valueType,
                transDate= TransactionToAdd.transDate,
                description= TransactionToAdd.description,
                fixedMonthly= TransactionToAdd.fixedMonthly,
                parentTransID= TransactionToAdd.parentTransID,
                tagID= TransactionToAdd.tagID
            };

            string insertTransQuery = "INSERT INTO transactions (transTitle,subCategoryID, transType, transValue, valueType, transDate, description, fixedMonthly, parentTransID, tagID) values (@transTitle,@subCategoryID, @transType, @transValue, @valueType, @transDate, @description, @fixedMonthly, @parentTransID, @tagID)";

            int newTransId = await _db.InsertReturnId(insertTransQuery, TransToAddParam);

            if (newTransId != 0)
            {
                object param = new
                {
                    id = newCategoryId,
                    userID = categoryToAdd.userID
                };

                string GetCategoryQuery = "SELECT id, categroyTitle, icon, color FROM categories WHERE id = @id AND userID = @userID";
                var recordsCategory = await _db.GetRecordsAsync<CategoryToShow>(GetCategoryQuery, param);
                CategoryToShow category = recordsCategory.FirstOrDefault();

                if (category != null)
                {
                    return Ok(category);
                }
                return BadRequest("category not found");

            }

            return BadRequest("category not created");
        }
    }
}
