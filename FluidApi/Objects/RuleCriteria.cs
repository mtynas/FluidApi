using System;
namespace FluidAutomationService.Objects
{
    public class RuleCriteria
    {
        public string testField     {get; set;}
        public string qualifier     {get; set;}
        public string ruleScope     {get; set;}

        public Int64 uid            {get; set;}
        public Int64 expression     {get; set;}
        public Int64 ruleUid        {get; set;}

    }
}
