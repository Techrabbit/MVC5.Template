﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Template.Objects
{
    public class Language : BaseModel
    {
        [Required]
        public String Abbreviation { get; set; }

        [Required]
        public String Name { get; set; }
    }
}