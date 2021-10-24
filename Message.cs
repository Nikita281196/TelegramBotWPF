using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotWPF
{
    struct Message
    {
        public string Time { get; set; }

        public long Id { get; set; }

        public string Msg { get; set; }

        public string FirstName { get; set; }

        public string Type { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }

        public Message(string Time, string Msg, string FirstName, long Id, string Type, string FileId, string FileName)
        {
            this.Time = Time;
            this.Msg = Msg;
            this.FirstName = FirstName;
            this.Id = Id;
            this.Type = Type;
            this.FileId = FileId;
            this.FileName = FileName;
        }
    }
}
