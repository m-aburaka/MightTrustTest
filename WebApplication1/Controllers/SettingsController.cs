using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class SettingsController : ApiController
    {
        private readonly Setting[] _settings =
        {
            new Setting {Key = "123", Value = "rarar ra"},
            new Setting {Key = "321", Value = "sdfasd sadf"},
            new Setting {Key = "333", Value = "ss"},
            new Setting {Key = "111", Value = "фадяв фдыва"},
            new Setting {Key = "222", Value = "фыа"},
            new Setting {Key = "333", Value = "kasdsad"},
            new Setting {Key = "45s", Value = "a"},
            new Setting {Key = "555 556", Value = "s"},
            new Setting {Key = "arr", Value = "s"},
            new Setting {Key = "tomato", Value = "s"},
            new Setting {Key = "no", Value = "d"},
            new Setting {Key = "yes", Value = "no"}
        };

        [Route("api/{searchfield}")]
        public IHttpActionResult GetItems(string searchfield)
        {
            //use http://localhost:1939/api/getitems?searchfield= to access this method

            var list = new List<Setting>();

            //if searchfield assigned, return matched items. else, return all items
            if (!string.IsNullOrEmpty(searchfield))
            {
                //split by space
                var searchValues = searchfield.Split(' ');
                //select setting where key (or part of it) exist in search query
                list.AddRange(_settings.Where(s => searchValues.Any(value => s.Key.Contains(value))));
            }
            else
            {
                list.AddRange(_settings);
            }

            return Ok(list);
        }
    }
}
