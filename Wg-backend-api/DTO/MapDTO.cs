﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.DTO
{
    public class MapDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string MapLocation { get; set; }
    }
}
