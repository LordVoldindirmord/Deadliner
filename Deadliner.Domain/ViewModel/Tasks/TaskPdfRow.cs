using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tasks
{
    public class TaskPdfRow
    {
        public string Title { get; set; } = null!;
        public string TagName { get; set; } = null!;
        public string TagColorHex { get; set; } = "#6b7280";
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Deadline { get; set; } = null!;
        public DateTime? DeadlineDate { get; set; }
    }
}
