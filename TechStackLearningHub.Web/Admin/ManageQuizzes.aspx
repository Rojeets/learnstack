<%@ Page Title="Manage Quizzes" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="ManageQuizzes.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageQuizzes" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
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

    <asp:Panel ID="pnlWorkspace" runat="server" Visible="false" CssClass="row g-3">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span>Quizzes</span>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="grdQuizzes" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0"
                        DataKeyNames="QuizID" OnRowCommand="grdQuizzes_RowCommand" EmptyDataText="No quizzes for this module.">
                        <Columns>
                            <asp:BoundField DataField="QuizTitle" HeaderText="Quiz" />
                            <asp:BoundField DataField="PassMarkPercent" HeaderText="Pass mark" DataFormatString="{0}%" />
                            <asp:TemplateField HeaderText="Time limit">
                                <ItemTemplate>
                                    <%# FormatDuration(Eval("DurationMinutes")) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="">
                                <ItemTemplate>
                                    <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-primary" Text="Edit" CommandName="EditQuiz" CommandArgument='<%# Eval("QuizID") %>' />
                                    <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-danger" Text="Delete" CommandName="DeleteQuiz" CommandArgument='<%# Eval("QuizID") %>'
                                        OnClientClick="return confirm('Delete this quiz and all its questions?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
        <div class="col-lg-4">
            <asp:Panel ID="pnlEditor" runat="server" CssClass="card">
                <div class="card-header">
                    <asp:Literal ID="litHeading" runat="server" />
                </div>
                <div class="card-body">
                    <asp:HiddenField ID="hidQuizId" runat="server" />
                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label" for="txtQuizTitle">Quiz title</label>
                            <asp:TextBox ID="txtQuizTitle" runat="server" CssClass="form-control" />
                        </div>
                        <div class="col-12">
                            <label class="form-label" for="txtPassMark">Pass mark (0-100)</label>
                            <asp:TextBox ID="txtPassMark" runat="server" CssClass="form-control" Text="50" />
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPassMark"
                                ValidationExpression="^(100|[1-9][0-9]|[0-9])$" ErrorMessage="Enter 0-100." CssClass="text-danger" Display="Dynamic" />
                        </div>
                        <div class="col-12">
                            <label class="form-label" for="txtDurationMinutes">Time limit (minutes)</label>
                            <asp:TextBox ID="txtDurationMinutes" runat="server" CssClass="form-control" />
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtDurationMinutes"
                                ValidationExpression="^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-3][0-9]|240)$"
                                ErrorMessage="Enter a time limit between 1 and 240 minutes."
                                CssClass="text-danger" Display="Dynamic" />
                            <div class="form-text">
                                The attempt is timed on the server. When the limit is reached the quiz is submitted
                                and graded automatically.
                            </div>
                        </div>
                    </div>
                    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-2" Visible="false" />
                    <div class="mt-3">
                        <asp:Button ID="btnSaveQuiz" runat="server" Text="Save quiz" CssClass="btn btn-primary" OnClick="btnSaveQuiz_Click" />
                        <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
                        <asp:Button ID="btnDeleteQuiz" runat="server" Text="Delete quiz" CssClass="btn btn-outline-danger" OnClick="btnDeleteQuiz_Click"
                            OnClientClick="return confirm('Delete this quiz and all its questions?');" />
                        <asp:HyperLink ID="lnkManageQuestions" runat="server" CssClass="btn btn-outline-info" Text="Manage questions &raquo;" />
                    </div>
                </div>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>
