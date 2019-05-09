$(document).ready(function () {
    $('.carousel').carousel();    
    let getSearchText = s => s.trim().length === 0 ? undefined : s;

    let app = new Vue({
        el: '#tabContent',
        data: {
            categoriesPageCount: 1,
        },
        mounted: function() {
            this.categoriesCallback(1);
            $(".onChangeCallback").change(() => this.categoriesCallback(1));
            $("#searchInput").keypress(this.searchCallback);
        },
        methods: {
            categoriesCallback: function(page) {
                let self = this;

                axios.get(util.apiUrl('/api/getQuizList', {
                    page: page,
                    size: global.quizCardAmount,
                    type: 'category',
                    difficulty: $("#difficultySelect option:selected").attr("value"),
                    categoryIndex: $("#categorySelect option:selected").attr("value"),
                    orderBy: $("#orderBySelect option:selected").attr("value"),
                    search: getSearchText($("#searchInput").val())
                })).then(function(response) {
                    let pageCount = self.renderList('#categoriesList', response.data);
                    if (pageCount > 1) self.categoriesPageCount = pageCount;
                    
                });
            },
            searchCallback: function(e) {
                if (e.keyCode === 13) {
                    this.categoriesCallback(1);
                }
            },
            renderList: function(id, data) {
                $(id).empty();
                let result = $(id).html(data);
                return parseInt($(result.find(".totalPages")[0]).attr("amount"));
            }
        }
    });
});