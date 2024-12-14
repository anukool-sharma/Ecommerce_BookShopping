using EOnego.DataAccess.Data;
using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOnego.DataAccess.Repository
{
    public class CategoryRepository:Repository<Category>,ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context):base(context)
        {
            _context = context;
        }
    }
}
