﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Template.Components.Adapters
{
    public class DateValidator : ModelValidator
    {
        public DateValidator(ModelMetadata metadata, ControllerContext context)
            : base(metadata, context)
        {
        }

        public override IEnumerable<ModelValidationResult> Validate(Object container)
        {
            return Enumerable.Empty<ModelValidationResult>();
        }
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            yield return new ModelClientValidationRule
            {
                ValidationType = "date",
                ErrorMessage = String.Format(Resources.Shared.Validation.FieldMustBeDate, Metadata.GetDisplayName().ToLower())
            };
        }
    }
}