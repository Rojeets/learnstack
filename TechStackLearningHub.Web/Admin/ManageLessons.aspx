<%@ Page Title="Manage Lessons" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="ManageLessons.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageLessons" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row g-3 mb-4">
        <div class="col-md-4">
            <asp:Label runat="server" AssociatedControlID="ddlCourse" CssClass="form-label" Text="Course" />
            <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
        </div>
        <div class="col-md-4">
            <asp:Label runat="server" AssociatedControlID="ddlModule" CssClass="form-label" Text="Module" />
            <asp:DropDownList ID="ddlModule" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlModule_SelectedIndexChanged" />
        </div>
    </div>

    <asp:Panel ID="pnlWorkspace" runat="server" Visible="false">
        <div class="row g-3">
            <div class="col-lg-8">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <span>Lessons</span>
                        <asp:HyperLink ID="lnkManageQuizzes" runat="server" Visible="false"
                            CssClass="btn btn-sm btn-outline-info">Module quizzes &raquo;</asp:HyperLink>
                    </div>
                    <div class="card-body p-0">
                        <asp:GridView ID="grdLessons" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0"
                            DataKeyNames="LessonID" OnRowCommand="grdLessons_RowCommand" EmptyDataText="No lessons in this module yet.">
                            <Columns>
                                <asp:TemplateField HeaderText="#">
                                    <ItemTemplate><%# Eval("LessonOrder") %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="LessonTitle" HeaderText="Title" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" Text="Edit" CommandName="EditLesson" CommandArgument='<%# Eval("LessonID") %>' CssClass="btn btn-sm btn-outline-primary" />
                                        <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteLesson" CommandArgument='<%# Eval("LessonID") %>' CssClass="btn btn-sm btn-outline-danger"
                                            OnClientClick="return confirm('Delete this lesson? Progress on it will be removed.');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
            <div class="col-lg-4">
                <div class="card">
                    <div class="card-header">
                        <h2 class="h5 mb-0" id="lblEditorHeading" runat="server">New lesson</h2>
                    </div>
                    <div class="card-body">
                        <asp:HiddenField ID="hidLessonId" runat="server" />
                        <div class="row g-3">
                            <div class="col-12">
                                <asp:Label runat="server" AssociatedControlID="txtLessonTitle" CssClass="form-label" Text="Lesson title" />
                                <asp:TextBox ID="txtLessonTitle" runat="server" CssClass="form-control" />
                            </div>
                            <div class="col-12">
                                <asp:Label runat="server" AssociatedControlID="txtContentHtml" CssClass="form-label" Text="Lesson content (HTML allowed)" />
                                <asp:TextBox ID="txtContentHtml" runat="server" TextMode="MultiLine" Rows="8" CssClass="form-control"
                                    placeholder="Introductory paragraphs, code samples, headings..." />
                            </div>
                            <div class="col-12">
                                <asp:Label runat="server" AssociatedControlID="txtVideoUrl" CssClass="form-label" Text="Video URL (YouTube embed / Vimeo player)" />
                                <asp:TextBox ID="txtVideoUrl" runat="server" CssClass="form-control" Placeholder="https://www.youtube.com/embed/..." />
                            </div>
                            <div class="col-12">
                                <asp:Label runat="server" AssociatedControlID="fupNotes" CssClass="form-label" Text="Notes file (PDF/DOCX/TXT)" />
                                <asp:FileUpload ID="fupNotes" runat="server" CssClass="form-control" />
                                <asp:Label ID="lblCurrentNotes" runat="server" CssClass="form-text d-block" />
                            </div>
                        </div>
                        <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-2" Visible="false" role="alert" />
                        <div class="mt-3">
                            <asp:Button ID="btnSave" runat="server" Text="Save lesson" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
