namespace MQService
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq;

    public class ModelMQData : DbContext
    {
        // 您的內容已設定為使用應用程式組態檔 (App.config 或 Web.config)
        // 中的 'ModelMQData' 連接字串。根據預設，這個連接字串的目標是
        // 您的 LocalDb 執行個體上的 'MQService.ModelMQData' 資料庫。
        // 
        // 如果您的目標是其他資料庫和 (或) 提供者，請修改
        // 應用程式組態檔中的 'ModelMQData' 連接字串。
        public ModelMQData()
            : base("name=ModelMQData")
        {
        }

        // 針對您要包含在模型中的每種實體類型新增 DbSet。如需有關設定和使用
        // Code First 模型的詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=390109。

        public virtual DbSet<MQData> MQDatas { get; set; }
    }

    public class MQData
    {
        [Key]
        [DisplayName(@"ID")]

        public Guid Id { get; set; }
        [Display(Name = @"放置日期/時間")]
        [DisplayName(@"放置日期/時間")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}")]
        public DateTime GetDate { get; set; }
        [Display(Name = "訊息ID")]
        [DisplayName(@"訊息ID")]
        public string MsgID { get; set; }
        [Display(Name = "相互關係ID")]
        [DisplayName(@"相互關係ID")]
        public string CorrelationId { get; set; }
        [Display(Name = "群組ID")]
        [DisplayName(@"群組ID")]
        public string GroupID { get; set; }
        [Display(Name = "佇列訊息")]
        [DisplayName(@"佇列訊息")]
        public string Msg { get; set; }

    }
}