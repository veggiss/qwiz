let quizFinished = false;
let page = 0;
let guessBtn;

gotoNext();
$("#nextBtn").click(gotoNext);
$(document).on("click", "#question .btn", answerQuestion);

function gotoNext() {
    if (quizFinished) {
        axios.get(`/api/getQuizSummary?id=${model.quizId}`).then(function (response) {
            $("#questionContent").html(response.data);
        }).catch(e => console.log(e));
    } else {
        axios.get(`/api/getQuestion?id=${model.quizId}&page=${page}`).then(function (response) {
            $("#questionContent").html(response.data);
            page++;
        }).catch(e => console.log(e));   
    }
}

function answerQuestion(e) {
    guessBtn = e.target;
    axios.get(util.apiUrl('/api/answer', {
        quizId: model.quizId,
        questionId: $("#question").attr("value"),
        guess: e.target.value
    })).then(function(response) {
        let answer = response.data.correctAnswer;
        quizFinished = response.data.quizFinished;
        console.log(quizFinished, response);
        let correctBtn = $("#question").find(`[value='${answer}']`)[0];
                
        $(correctBtn).css("background-color", "#28a745");
        
        if (guessBtn.value !== answer) {
            $(guessBtn).css("background-color", "#dc3545");
        }
    });
}


//$(document).ready(() => {    
//    model.questions.forEach(e => {
//        if (e.questionType === "multiple_choice") e.alternatives = JSON.parse(e.alternatives);
//        else e.alternatives = ['', '', '', ''];
//    });
//
//    let app = new Vue({
//        el: '#app',
//        data: {
//            quiz: model,
//            guessed: false,
//            page: 0,
//            correctAnswers: 0,
//            resultMsg: '',
//            finished: false
//        },
//        mounted: function() {
//            this.renderQuestion();
//        },
//        methods: {
//            answer: function(alt) {
//                let self = this;
//                
//                if (!self.guessed && !self.finished) {
//                    let questionId = this.quiz.questions[this.page].id;
//                    let quizId = this.quiz.id;
//
//                    axios.get(`/api/answer?quizId=${quizId}&questionId=${questionId}&guess=${alt}`).then(function(response) {
//                        let answer = response.data.correctAnswer;
//                        $("#btn_" + answer).css("background-color", "#28a745");
//                        if (answer !== alt) self.wrongAnswer(alt);
//                        else self.correctAnswer();
//
//                        self.guessed = true;
//                    });
//                }
//            },
//            next: function() {
//                if (this.guessed) {
//                    if (this.page < this.quiz.questions.length - 1) {
//                        ['#btn_A', '#btn_B', '#btn_C', '#btn_D', '#btn_true', '#btn_false'].forEach(
//                            e => $(e).css("background-color", "transparent"));
//                        this.page++;
//                        this.guessed = false;
//                    } else {
//                        this.resultMsg = `${this.correctAnswers} of ${this.quiz.questions.length} questions correct!`;
//                        this.finished = true;
//                    }
//
//                    this.renderQuestion();
//                }
//            },
//            renderQuestion: function() {
//                if (this.quiz.questions[this.page].questionType === "multiple_choice") {
//                    $(".multiple_choice").show();
//                    $(".true_false").hide();
//                } else {
//                    $(".multiple_choice").hide();
//                    $(".true_false").show();
//                }
//            },
//            wrongAnswer: function(alt) {
//                $("#progressBar").append(`<div class="progress-bar bg-danger progress-bar-striped progress-bar-animated" role="progressbar" style="width: ${1 / model.questions.length * 100}%"></div>`);
//                $("#btn_" + alt).css("background-color", "#dc3545");
//            },
//            correctAnswer: function() {
//                this.correctAnswers++;
//                $("#progressBar").append(`<div class="progress-bar bg-success progress-bar-striped progress-bar-animated" role="progressbar" style="width: ${1 / model.questions.length * 100}%"></div>`);
//            }
//        }
//    });
//});