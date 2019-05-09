$(document).ready(() => {
    $(".progress-bar").css("width", ((model.xp) / (model.xpNeeded)) * 100 + "%");
    
    let app = new Vue({
        el: '#tabContent',
        data: {
            historyPageCount: 1,
            myQuizzesPageCount: 1,
            badgesPageCount: 1
        },
        mounted: function() {
            this.historyCallback(1);
            this.myQuizzesCallback(1);
        },
        methods: {
            historyCallback: function(page) {
                let self = this;
                
                axios.get(util.apiUrl('/api/getQuizList', {
                    page: page,
                    size: global.quizCardAmount,
                    type: 'history',
                    username: model.userName
                })).then(function(response) {
                    let pageCount = self.renderList('#historyList', response.data);
                    if (pageCount > 1) self.historyPageCount = pageCount;
                });
            },
            myQuizzesCallback: function(page) {
                let self = this;
                
                axios.get(util.apiUrl('/api/getQuizList', {
                    page: page,
                    size: global.quizCardAmount,
                    type: 'quizzesBy',
                    username: model.userName
                })).then(function(response) {
                    let pageCount = self.renderList('#myQuizzesList', response.data);
                    if (pageCount > 1) self.myQuizzesPageCount = pageCount;
                });
            },
            badgesCallback: function(page) {

            },
            renderList: function(id, data) {
                $(id).empty();
                let result = $(id).html(data);
                return parseInt($(result.find(".totalPages")[0]).attr("amount"));
            }
        }
    });
});