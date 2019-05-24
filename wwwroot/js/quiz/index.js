$(document).ready(function () {
    $('.carousel').carousel();    
    let getSearchText = s => s.trim().length === 0 ? undefined : s;

    let app = new Vue({
        el: '#tabContent',
        data: {
            searchEntries: {
                entries: [],
                pages: 0
            }
        },
        mounted: function() {
            this.categoriesCallback(1);
            $(".onChangeCallback").change(() => this.categoriesCallback(1));
            $("#searchInput").keypress(this.searchCallback);
        },
        methods: {
            categoriesCallback: function(page) {
                let self = this;

                axios.get(util.apiUrl(`/api/quiz/getList/search`, {
                    page: page,
                    size: global.quizCardAmount,
                    difficulty: $("#difficultySelect option:selected").attr("value"),
                    categoryIndex: $("#categorySelect option:selected").attr("value"),
                    orderBy: $("#orderBySelect option:selected").attr("value"),
                    search: getSearchText($("#searchInput").val())
                })).then(function(response) {
                    if (global.debug) util.logResponse(response);
                    self.searchEntries.entries = response.data.entries;
                    self.searchEntries.pages = response.data.pages;
                }).catch(function(e) {
                    util.logResponse(e);
                });
            },
            searchCallback: function(e) {
                if (e.keyCode === 13) {
                    this.categoriesCallback(1);
                }
            }
        }
    });
});