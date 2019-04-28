using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.DataFederation;
using DevExpress.DataAccess.Excel;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.Configuration;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BindReportToFederatedMasterDetailDataSource {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        void Button1_Click(object sender, EventArgs e) {
            ReportDesignTool designTool = new ReportDesignTool(CreateReport());
            designTool.ShowRibbonDesignerDialog();
        }

        static FederationDataSource CreateFederationDataSource(SqlDataSource sql, ExcelDataSource excel) {
            Source sqlSource = new Source(sql.Name, sql, "Categories");
            Source excelSource = new Source(excel.Name, excel, "");
            // Select the required columns from 
            //the Sql Source for the Federation query result
            var categoriesNode = sqlSource.From()
                .Select("CategoryID", "CategoryName", "Description").Build("Categories");
            // Select the required columns 
            //from the Excel Source for the Federation query result
            var productsNode = excelSource.From()
                .Select("ProductName", "CategoryID", "UnitPrice").Build("Products");

            var federationDataSource = new FederationDataSource();
            federationDataSource.Queries.AddRange(new[] { categoriesNode, productsNode });
            var relation = new FederationMasterDetailInfo("Categories", "Products", new FederationRelationColumnInfo("CategoryID", "CategoryID"));
            federationDataSource.Relations.Add(relation);
            federationDataSource.RebuildResultSchema();
            return federationDataSource;
        }

        public static XtraReport CreateReport() {
            var report = new XtraReport();
            var detailBand = new DetailBand() { HeightF = 25 };
            var detailReport = new DetailReportBand();
            var detailBand2 = new DetailBand() { HeightF = 25 };
            detailReport.Bands.Add(detailBand2);
            report.Bands.AddRange(new Band[] { detailBand, detailReport });

            var categoryLabel = new XRLabel() { WidthF = 150 };
            var productLabel = new XRLabel() { WidthF = 300, LocationF = new PointF(100, 0) };
            categoryLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[CategoryName]"));
            productLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[ProductName]"));
            detailBand.Controls.Add(categoryLabel);
            detailBand2.Controls.Add(productLabel);

            var sqlDataSource = CreateSqlDataSource();
            var excelDataSource = CreateExcelDataSource();
            var federationDataSource = CreateFederationDataSource(sqlDataSource, excelDataSource);
            report.ComponentStorage.AddRange(new IComponent[] { sqlDataSource, excelDataSource, federationDataSource });
            report.DataSource = federationDataSource;
            report.DataMember = "Categories";
            detailReport.DataSource = federationDataSource;
            detailReport.DataMember = "Categories.CategoriesProducts";

            return report;
        }

        static SqlDataSource CreateSqlDataSource() {
            var connectionParameters = new Access97ConnectionParameters("Data/nwind.mdb", "", "");
            var sqlDataSource = new SqlDataSource(connectionParameters) { Name = "Sql_Categories" };
            var categoriesQuery = SelectQueryFluentBuilder.AddTable("Categories").SelectAllColumnsFromTable().Build("Categories");
            sqlDataSource.Queries.Add(categoriesQuery);
            sqlDataSource.RebuildResultSchema();
            return sqlDataSource;
        }

        static ExcelDataSource CreateExcelDataSource() {
            var excelDataSource = new ExcelDataSource() { Name = "Excel_Products" };
            excelDataSource.FileName = "Data/Products.xlsx";
            excelDataSource.SourceOptions = new ExcelSourceOptions() {
                ImportSettings = new ExcelWorksheetSettings("Sheet"),
            };
            excelDataSource.RebuildResultSchema();
            return excelDataSource;
        }
    }
}
