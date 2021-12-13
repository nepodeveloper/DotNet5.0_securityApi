using Catalog.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController: ControllerBase
    {
        private readonly ItemsRepository repository;

        public ItemController(ItemsRepository itemsRepository)
        {
            repository = itemsRepository;
        }


    }
}
