﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Programmin2_classroom.Shared.Models.present.toAdd;
using Programmin2_classroom.Shared.Models.present.toEdit;
using Programmin2_classroom.Shared.Models.present.toShow;
using TriangleDbRepository;

namespace Programmin2_classroom.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresentController : ControllerBase
    {
        private readonly DbRepository _db;

        public PresentController(DbRepository db)
        {
            _db = db;
        }

        [HttpGet("userToShow/{userID}")] // שליפה של התצוגה הראשונית בסביבה השניה לפני לחיצות על כפתורים
        public async Task<IActionResult> GetUser(int userID)
        {
            // Initialize the SQL queries
            var userQuery = "SELECT id, firstName, profilePicOrIcon FROM users WHERE id = @ID";
            var categoryQuery = "SELECT id, categroyTitle, icon, color FROM categories WHERE userID = @ID";
            var subCategoryBudgetQuery = "SELECT COALESCE(SUM(monthlyPlannedBudget), 0) FROM subcategories WHERE categoryID = @ID";
            var transactionSumQuery = "SELECT COALESCE(SUM(transValue), 0) FROM transactions WHERE subCategoryID = @ID AND transType = @TransType";

            // Get user details
            var user = (await _db.GetRecordsAsync<userToShow>(userQuery, new { ID = userID })).FirstOrDefault();
            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Get categories for the user
            var categories = (await _db.GetRecordsAsync<CategoryToShow>(categoryQuery, new { ID = userID })).ToList();
            if (categories.Any())
            {
                user.categoriesFullList = categories;

                double totalBudget = 0;
                foreach (var category in categories)
                {
                    // Get total budget for each category
                    double categoryBudget = (await _db.GetRecordsAsync<double>(subCategoryBudgetQuery, new { ID = category.id })).FirstOrDefault();
                    totalBudget += categoryBudget;

                    if (category.id == categories.First().id) // Assuming you want to calculate transactions for the first category only.
                    {
                        // Calculate transaction sums for the first category
                        user.spendingValueFullList = (await _db.GetRecordsAsync<double>(transactionSumQuery, new { ID = category.id, TransType = 1 })).FirstOrDefault();
                        user.incomeValueFullList = (await _db.GetRecordsAsync<double>(transactionSumQuery, new { ID = category.id, TransType = 2 })).FirstOrDefault();
                    }
                }

                // Calculate the budget usage percentage
                user.budgetFullValue = CalculateBudgetPercentage(totalBudget, user.spendingValueFullList);
            }

            return Ok(user);
        }

        private double CalculateBudgetPercentage(double budget, double spending)
        {
            if (spending == 0)
                return 0;
            return Math.Round((budget / spending) * 100, 2);
        }


        [HttpGet("subCategoryToShow/{categoryID}")] // הצגה של תתי קטגוריות בלחיצה על הדרופדאון
        public async Task<IActionResult> subCategoryToShow(int categoryID)
        {
            object param = new
            {
                ID = categoryID
            };

             // צריך להוסיף שאילתה שמושכת את הצבע מהקטגוריה בשביל להציג גם בתתי קטגוריות אם בחר צבע.

            string GetSubCategoriesQuery = "SELECT id, subCategoryTitle, monthlyPlannedBudget FROM subcategories WHERE categoryID = @ID";
            var recordsubCategories = await _db.GetRecordsAsync<SubCategoryToShow>(GetSubCategoriesQuery, param);
            List<SubCategoryToShow> subCategories = recordsubCategories.ToList();

            if (subCategories.Count > 0)
            {
                foreach (var subCategory in subCategories)
                {
                    if (subCategory.id != null)
                    {
                        object subParam = new
                        {
                            ID = subCategory.id
                        };

                        string GetTransactionValueQuery = "SELECT COALESCE(SUM(transValue), 0) FROM transactions WHERE subCategoryID = @ID AND transType = 1";

                        var recordsTransValue = await _db.GetRecordsAsync<double>(GetTransactionValueQuery, subParam);
                        subCategory.transactionsValue = recordsTransValue.FirstOrDefault();
                    }
                    else
                    {
                        return BadRequest("no transaction in sub category");
                    }

                }

                return Ok(subCategories);
            }

            return BadRequest("user not found");
        }


        [HttpGet("GetCategoryToEdit/{CategoryId}")] // שליפת קטגוריה לעריכה
        public async Task<IActionResult> GetFullGame(int CategoryId)
        {

            object param = new
            {
                ID = CategoryId
            };

            string GetCategoryQuery = "SELECT id, categroyTitle, icon, color FROM categories WHERE id = @ID";

            var recordCategory = await _db.GetRecordsAsync<CategoryToShow>(GetCategoryQuery, param);
            CategoryToShow category = recordCategory.FirstOrDefault();

            if (category != null)
            {
                return Ok(category);
            }
            return BadRequest("category not found");
        }


        [HttpPost("EditCategory")]  // עריכת קטגוריה

        public async Task<IActionResult> editCategory(CategoryToEdit categoryToUpdate)
        {

            object updateParam = new
            {
                ID = categoryToUpdate.id,
                categroyTitle = categoryToUpdate.categroyTitle,
                icon = categoryToUpdate.icon,
                color = categoryToUpdate.color
            };

            string UpdateCategoryQuery = "UPDATE categories set categroyTitle = @categroyTitle, icon = @icon, color = @color where id =@ID";
            bool isUpdate = await _db.SaveDataAsync(UpdateCategoryQuery, updateParam);

            if (isUpdate)
            {
                return Ok(categoryToUpdate);
            }
            return BadRequest("update category faild");
        }



        [HttpPost("AddCategory/{userID}")] // יצירת קטגוריה חדשה
        public async Task<IActionResult> AddCategory(int userID, CategoryToAdd categoryToAdd) // לראות איך היוזר אידי מגיע מהפרונט אנד 
        {
            object categoryToAddParam = new
            {
                userID = userID,
                categroyTitle = categoryToAdd.categroyTitle,
                icon = categoryToAdd.icon,
                color = categoryToAdd.color
            };

            string insertCategoryQuery = "INSERT INTO categories (categroyTitle,userID, icon, color) values (@categroyTitle ,@userID ,@icon ,@color)";

            int newCategoryId = await _db.InsertReturnId(insertCategoryQuery, categoryToAddParam);

            if (newCategoryId != 0)
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

        [HttpDelete("deleteCategory/{CategoryIdToDelete}")] // מחיקת קטגוריה
        public async Task<IActionResult> DeleteCategory(int CategoryIdToDelete)
        {
            string DeleteQuery = "DELETE FROM categories WHERE id=@ID";
            bool isCategoryDeleted = await _db.SaveDataAsync(DeleteQuery, new { ID = CategoryIdToDelete });

            if (isCategoryDeleted)
            {
                return Ok();
            }

            return BadRequest("Failed to delete category");
        }


        [HttpGet("GetCategoriesOverview/{userID}")] // שליפת קטגוריות וסכומים לעמוד סטורי 3 במצב החודש כרגע
        public async Task<IActionResult> GetCategoriesOverview(int userID)
        {
            object param = new { ID = userID };

            string GetCategoryOverviewIDQuery = "SELECT id FROM categories WHERE userID = @ID";
            var recordCategoryOverviewID = await _db.GetRecordsAsync<int>(GetCategoryOverviewIDQuery, param);

            if (recordCategoryOverviewID == null)
            {
                return Ok(new List<CategoriesOverviewToShow>());  // Return an empty list if no categories found
            }

            List<CategoriesOverviewToShow> categoriesOverviewToShowList = new List<CategoriesOverviewToShow>();

            foreach (int catID in recordCategoryOverviewID)
            {
                double subCatSum = 0;  // Reset sum for each category

                object categoryParam = new { ID = catID };


                string GetSubCategoryIDQuery = "SELECT id FROM subcategories WHERE categoryID = @ID";
                var recordSubCategoryID = await _db.GetRecordsAsync<int>(GetSubCategoryIDQuery, categoryParam);

                if (recordSubCategoryID != null)
                {
                    foreach (int subCatID in recordSubCategoryID)
                    {
                        object subCatIDParam = new { ID = subCatID };

                        // Expenses:
                        string GetCategoryCurrentSumQuery = "SELECT COALESCE(SUM(transValue), 0) FROM transactions WHERE subCategoryID = @ID";
                        var recordSubCatCurrentSum = await _db.GetRecordsAsync<double>(GetCategoryCurrentSumQuery, subCatIDParam);
                        subCatSum += recordSubCatCurrentSum.FirstOrDefault();

                        //    // Income:
                        //    string GetIncomesQuery = "SELECT COALESCE(SUM(transValue), 0) FROM transactions WHERE subCategoryID = @ID AND transType = 2";
                        //    var recordSubCatCurrentSumIncome = await _db.GetRecordsAsync<double>(GetIncomesQuery, subCatIDParam);
                        //    subCatSum += recordSubCatCurrentSumIncome.FirstOrDefault();
                        //}
                    }

                    string GetCategoryTitleOverviewQuery = "SELECT categroyTitle FROM categories WHERE id = @ID"; // Correct potential typo from 'categroyTitle' to 'categoryTitle'
                    var getCategoryTitle = await _db.GetRecordsAsync<string>(GetCategoryTitleOverviewQuery, categoryParam);
                    CategoriesOverviewToShow currentCategoryStats = new CategoriesOverviewToShow();

                    if (getCategoryTitle != null)
                    {                     
                        currentCategoryStats.categroyTitle = getCategoryTitle.FirstOrDefault(); // Also corrected the property name if typo existed
                        currentCategoryStats.currentCategorySum = subCatSum;                        
                    }

                    categoriesOverviewToShowList.Add(currentCategoryStats);
                }                
            }
            return Ok(categoriesOverviewToShowList);
        }


        [HttpGet("GetTagsAndSpendings/{userID}")] // שליפת תגיות וסכומים שלהן לעמוד סטורי 2 במצב החודש כרגע
        public async Task<IActionResult> GetTagsAndSpendings(int userID)
        {
            List<TagsAndSpendingsToShow> TagsAndSpendingsToShowList = new List<TagsAndSpendingsToShow>();

            object param = new
            {
                ID = userID
            };

            string GetTagsAndSpendingsQuery = @"
        SELECT t.tagID, tg.tagTitle, tg.tagColor, SUM(t.transValue) as totalValue 
        FROM transactions t
        JOIN tags tg ON t.tagID = tg.id
        JOIN subcategories sc ON t.subCategoryID = sc.id
        JOIN categories c ON sc.categoryID = c.id
        WHERE c.userID = @ID AND t.transType = 1 
        GROUP BY t.tagID, tg.tagTitle, tg.tagColor 
        ORDER BY totalValue DESC 
        LIMIT 3";

            var recordTagsAndSpendings = await _db.GetRecordsAsync<TagsAndSpendingsToShow>(GetTagsAndSpendingsQuery, param);
            TagsAndSpendingsToShowList = recordTagsAndSpendings.ToList();

            if (TagsAndSpendingsToShowList != null && TagsAndSpendingsToShowList.Any())
            {
                return Ok(TagsAndSpendingsToShowList);
            }
            return BadRequest("Tags and Spendings not found");
        }

        // לעשות שיטה של הסטורי הראשון !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        [HttpGet("GetSubCategoryToEdit/{SubCategoryId}")] // שליפת תת קטגוריה לעריכה
        public async Task<IActionResult> GetSubCategoryToEdit(int SubCategoryId)
        {
            object param = new
            {
                ID = SubCategoryId
            };

            // Updated SQL query to join the subcategories table with the category table
            string GetSubCategoryQuery = @"
        SELECT sc.id, sc.subCategoryTitle, sc.categoryID, sc.monthlyPlannedBudget, sc.importance, cat.categroyTitle 
        FROM subcategories sc
        JOIN categories cat ON sc.categoryID = cat.id
        WHERE sc.id = @ID";

            var recordSubCategory = await _db.GetRecordsAsync<SubCategoryToEdit>(GetSubCategoryQuery, param);
            SubCategoryToEdit subCategory = recordSubCategory.FirstOrDefault();

            if (subCategory != null)
            {
                return Ok(subCategory);
            }
            return BadRequest("Sub Category not found");
        }

        [HttpPost("EditSubCategory")]  // עריכת  תת קטגוריה

        public async Task<IActionResult> editSubCategory(SubCategoryToUpdate subCategoryToUpdate)
        {

            object subCategoryUpdateParam = new
            {
                ID = subCategoryToUpdate.id,
                subCategoryTitle = subCategoryToUpdate.subCategoryTitle,
                categoryID = subCategoryToUpdate.categoryID,
                monthlyPlannedBudget = subCategoryToUpdate.monthlyPlannedBudget,
                importance = subCategoryToUpdate.importance
            };

            string UpdateSubCategoryQuery = "UPDATE subcategories set subCategoryTitle = @subCategoryTitle, categoryID = @categoryID, monthlyPlannedBudget = @monthlyPlannedBudget, importance=@importance where id =@ID";
            bool isUpdate = await _db.SaveDataAsync(UpdateSubCategoryQuery, subCategoryUpdateParam);

            if (isUpdate)
            {
                return Ok(subCategoryToUpdate);
            }
            return BadRequest("update sub category faild");
        }

        [HttpPost("AddSubCategory")] // יצירת תת קטגוריה חדשה
        public async Task<IActionResult> AddSubCategory(SubCategoryToAdd subCategoryToAdd) 
        {
            object subCategoryToAddParam = new
            {
                categoryID = subCategoryToAdd.categoryID,
                subCategoryTitle = subCategoryToAdd.subCategoryTitle,
                monthlyPlannedBudget = subCategoryToAdd.monthlyPlannedBudget,
                importance = subCategoryToAdd.importance
            };

            string insertSubCategoryQuery = "INSERT INTO subcategories (categoryID,subCategoryTitle,monthlyPlannedBudget, importance) values (@categoryID ,@subCategoryTitle ,@monthlyPlannedBudget ,@importance)";

            int newSubCategoryId = await _db.InsertReturnId(insertSubCategoryQuery, subCategoryToAddParam);

            if (newSubCategoryId != 0)
            {
                object param = new
                {
                    id = newSubCategoryId
                };

                string GetSubCategoryQuery = "SELECT id,categoryID,subCategoryTitle, monthlyPlannedBudget, importance FROM subcategories WHERE id = @id";
                var recordsSubCategory = await _db.GetRecordsAsync<SubCategoryToAdd>(GetSubCategoryQuery, param);
                SubCategoryToAdd subCategory = recordsSubCategory.FirstOrDefault();

                if (subCategory != null)
                {
                    return Ok(subCategory);
                }
                return BadRequest("sub category not found");

            }

            return BadRequest("sub category not created");
        }

        [HttpDelete("deleteSubCategory/{SubCategoryIdToDelete}")] // מחיקת תת קטגוריה
        public async Task<IActionResult> DeleteSubCategory(int SubCategoryIdToDelete)
        {
            string DeleteQuery = "DELETE FROM subcategories WHERE id=@ID";
            bool isSubCategoryDeleted = await _db.SaveDataAsync(DeleteQuery, new { ID = SubCategoryIdToDelete });

            if (isSubCategoryDeleted)
            {
                return Ok();
            }

            return BadRequest("Failed to delete sub category");
        }

        [HttpGet("GetUserCategories/{userID}")] // שליפת תת קטגוריה לעריכה
        public async Task<IActionResult> GetUserCategories(int userID)
        {

            object param = new
            {
                ID = userID
            };

            var usercategoriesQuery = "SELECT id, categroyTitle FROM categories WHERE userID = @ID AND categroyTitle != 'הכנסות';";
            var recordusercategoriesQuery = await _db.GetRecordsAsync<AllUserCategories>(usercategoriesQuery, param);
            List<AllUserCategories> allUserCategories = recordusercategoriesQuery.ToList();

            if (allUserCategories != null)
            {
                return Ok(allUserCategories);
            }
            return BadRequest("Category not found");
        }





    }
}




