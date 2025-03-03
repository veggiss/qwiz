$(document).ready(() => {
    new Vue({
        el: '#app',
        data: {
            getBars: () => $("#progressBars").children(),
            isAuthenticated: global.isAuthenticated,
            lastQuestion: false,
            answered: false,
            state: 'intro',
            question: null,
            quiz: model,
            xpGained: 0,
            xpGainedUnmodified: 0,
            timer: 0,
            page: 0
        },
        methods: {
            onNextQuestion: function() {
                if (this.page < this.quiz.questions.length - 1) {
                    $("#question").find('button').removeClass('alert-danger bg-success');
                    this.answered = false;
                    this.page++;
                    
                    this.updateQuestion();
                    this.startTimer();
                }
            },
            onAnswerQuestion: function(e) {
                if (!this.answered) {
                    let self = this;
                    
                    console.log(global.header);
                    
                    axios.put(util.apiUrl('/api/question/answer', {
                        quizId: this.quiz.id,
                        questionId: this.question.id,
                        guessAlternative: e.target.name
                    })).then(function(response) {
                        if (global.debug) util.logResponse(response);
                        let correctAlternative = response.data.correctAlternative;
                        let wasNotCorrect = e.target.name !== correctAlternative;
                        let bonus = response.data.bonus;
                        let xp = response.data.xpGained;
                        let bars = self.getBars();
                        self.answered = true;
                        self.stopTimer();
                        $("#nextBtnCollapse").collapse('show');
                        $("#question").find(`button[name='${correctAlternative}']`).addClass('bg-success');
                        
                        if (wasNotCorrect) {
                            $(e.target).addClass('alert-danger');
                            $(bars[self.page]).addClass('bg-danger');
                        } else {
                            $(bars[self.page]).addClass('bg-success');
                            self.addXpPoints(xp + bonus);
                        }
                    }).catch(e => util.logResponse(e.response));
                }
            },
            updateQuestion: function() {
                this.question = this.quiz.questions[this.page];
                this.question.alternatives = this.question.alternatives == null ? null : JSON.parse(this.question.alternatives);
                if (this.quiz.questions.length - 1 <= this.page) this.lastQuestion = true;
            },
            onNextState: function(state) {
                this.state = state;
                
                if (state === "question") {
                    this.startTimer();
                } else if (state === "summary") {
                    window.location.href = '/quiz/summary/' + this.quiz.id;
                }
            },
            startTimer: function() {
                $('#progressTimer').css('width', '100%');
                $("#nextBtnCollapse").collapse('hide');
                if (global.isAuthenticated) axios.put("/api/question/startTimer");
                this.stopTimer();
                
                this.timer = 15;
                let countDown = 0;
                let self = this;

                this.counter = setInterval(function () {
                    countDown++;
                    
                    if (countDown <= 15) {
                        self.timer--;
                        let percent = (15 - countDown) / 15 * 100;
                        $('#progressTimer').css('width', percent + '%');
                    } else {
                        self.stopTimer();
                    }

                }, 1000);
            },
            stopTimer: function() {
                if (this.counter !== null) clearInterval(this.counter);
            },
            addXpPoints: function(xp) {
                let self = this;
                this.xpGainedUnmodified += xp;
                $({count: self.xpGained}).animate({count: self.xpGainedUnmodified + 1}, {
                    duration:2000,
                    easing:'swing',
                    step: function() {
                        self.xpGained = Math.floor(this.count);
                    }
                });
            }
        },
        created: function () {
            this.updateQuestion();
        }
    })
});