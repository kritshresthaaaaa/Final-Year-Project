using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models
{

    public class MinWordsAttribute : ValidationAttribute
    {
        private readonly int _minWords;

        public MinWordsAttribute(int minWords) : base($"The description must be at least {minWords} words.")
        {
            _minWords = minWords;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var valueAsString = value.ToString();
                if (valueAsString.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < _minWords)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
