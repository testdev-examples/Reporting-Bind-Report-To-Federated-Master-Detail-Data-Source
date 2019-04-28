Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.DataAccess.DataFederation
Imports DevExpress.DataAccess.Excel
Imports DevExpress.DataAccess.Sql
Imports DevExpress.XtraReports.Configuration
Imports DevExpress.XtraReports.UI
Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace BindReportToFederatedMasterDetailDataSource
    Partial Public Class Form1
        Inherits Form

        Public Sub New()
            InitializeComponent()
        End Sub
        Private Sub Button1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button1.Click
            Dim designTool As New ReportDesignTool(CreateReport())
            designTool.ShowRibbonDesignerDialog()
        End Sub

        Private Shared Function CreateFederationDataSource(ByVal sql As SqlDataSource, ByVal excel As ExcelDataSource) As FederationDataSource
            Dim sqlSource As New Source(sql.Name, sql, "Categories")
            Dim excelSource As New Source(excel.Name, excel, "")
            ' Select the required columns from 
            'the Sql Source for the Federation query result
            Dim categoriesNode = sqlSource.From().Select("CategoryID", "CategoryName", "Description").Build("Categories")
            ' Select the required columns 
            'from the Excel Source for the Federation query result
            Dim productsNode = excelSource.From().Select("ProductName", "CategoryID", "UnitPrice").Build("Products")

            Dim federationDataSource = New FederationDataSource()
            federationDataSource.Queries.AddRange( { categoriesNode, productsNode })
            Dim relation = New FederationMasterDetailInfo("Categories", "Products", New FederationRelationColumnInfo("CategoryID", "CategoryID"))
            federationDataSource.Relations.Add(relation)
            federationDataSource.RebuildResultSchema()
            Return federationDataSource
        End Function

        Public Shared Function CreateReport() As XtraReport
            Dim report = New XtraReport()
            Dim detailBand = New DetailBand() With {.HeightF = 25}
            Dim detailReport = New DetailReportBand()
            Dim detailBand2 = New DetailBand() With {.HeightF = 25}
            detailReport.Bands.Add(detailBand2)
            report.Bands.AddRange(New Band() { detailBand, detailReport })

            Dim categoryLabel = New XRLabel() With {.WidthF = 150}
            Dim productLabel = New XRLabel() With { _
                .WidthF = 300, _
                .LocationF = New PointF(100, 0) _
            }
            categoryLabel.ExpressionBindings.Add(New ExpressionBinding("BeforePrint", "Text", "[CategoryName]"))
            productLabel.ExpressionBindings.Add(New ExpressionBinding("BeforePrint", "Text", "[ProductName]"))
            detailBand.Controls.Add(categoryLabel)
            detailBand2.Controls.Add(productLabel)

            Dim sqlDataSource = CreateSqlDataSource()
            Dim excelDataSource = CreateExcelDataSource()
            Dim federationDataSource = CreateFederationDataSource(sqlDataSource, excelDataSource)
            report.ComponentStorage.AddRange(New IComponent() { sqlDataSource, excelDataSource, federationDataSource })
            report.DataSource = federationDataSource
            report.DataMember = "Categories"
            detailReport.DataSource = federationDataSource
            detailReport.DataMember = "Categories.CategoriesProducts"

            Return report
        End Function

        Private Shared Function CreateSqlDataSource() As SqlDataSource
            Dim connectionParameters = New Access97ConnectionParameters("Data/nwind.mdb", "", "")
            Dim sqlDataSource = New SqlDataSource(connectionParameters) With {.Name = "Sql_Categories"}
            Dim categoriesQuery = SelectQueryFluentBuilder.AddTable("Categories").SelectAllColumnsFromTable().Build("Categories")
            sqlDataSource.Queries.Add(categoriesQuery)
            sqlDataSource.RebuildResultSchema()
            Return sqlDataSource
        End Function

        Private Shared Function CreateExcelDataSource() As ExcelDataSource
            Dim excelDataSource = New ExcelDataSource() With {.Name = "Excel_Products"}
            excelDataSource.FileName = "Data/Products.xlsx"
            excelDataSource.SourceOptions = New ExcelSourceOptions() With {.ImportSettings = New ExcelWorksheetSettings("Sheet")}
            excelDataSource.RebuildResultSchema()
            Return excelDataSource
        End Function
    End Class
End Namespace
