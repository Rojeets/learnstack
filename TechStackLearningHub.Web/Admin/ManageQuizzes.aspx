<%@ Page Title="Manage Quizzes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageQuizzes.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageQuizzes" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Manage Quizzes</h1>

    <div class="row g-3 mb-4">
        <div class="col-md-4">
            <label class="form-label" for="ddlCourse">Course</label>
            <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
        </div>
        <div class="col-md-4">
            <label class="form-label" for="ddlModule">Module</label>
            <asp:DropDownList ID="ddlModule" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlModule_SelectedIndexChanged" />
        </div>
    </div>

    <asp:Panel ID="pnlWorkspace" runat="server" Visible="false">
        <div class="card">
            <div class="card-body">
                <h5 class="card-title">
                    <asp:Literal ID="litHeading" runat="server" />
                </h5>
                <asp:Panel ID="pnlEditor" runat="server">
                    <asp:HiddenField ID="hidQuizId" runat="server" />
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label" for="txtQuizTitle">Quiz title</label>
                            <asp:TextBox ID="txtQuizTitle" runat="server" CssClass="form-control" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label" for="txtPassMark">Pass mark (0-100)</label>
                            <asp:TextBox ID="txtPassMark" runat="server" CssClass="form-control" Text="50" />
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPassMark"
                                ValidationExpression="^(100|[1-9]?[0-9])$" ErrorMessage="Enter 0-100." CssClass="text-danger" Display="Dynamic" />
                        </div>
                    </div>
                    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-2" Visible="false" />
                    <div class="mt-3">
                        <asp:Button ID="btnSaveQuiz" runat="server" Text="Save quiz" CssClass="btn btn-primary" OnClick="btnSaveQuiz_Click" />
                        <asp:Button ID="btnDeleteQuiz" runat="server" Text="Delete quiz" CssClass="btn btn-outline-danger" OnClick="btnDeleteQuiz_Click"
                            OnClientClick="return confirm('Delete this quiz and all its questions?');" />
                        <asp:HyperLink ID="lnkManageQuestions" runat="server" CssClass="btn btn-outline-info" Text="Manage questions &raquo;" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </asp:Panel>
</asp:Content>