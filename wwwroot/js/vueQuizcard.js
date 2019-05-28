Vue.component('quizcard', {
    props: ['id', 'quiz', 'canEdit', 'showSummary', 'quizTaken'],
    template:
`
<div class="card float-left m-2 w-100" style="max-width: 22rem;">
    <div class="row no-gutters">
        <div class="col-5">
            <img class="quizAvatar img-fluid" :src=quiz.imagePath>
            <div class="w-100 position-absolute text-light" style="bottom: 0; background: rgba(0, 0, 0, 0.5);">
                <div v-if="quizTaken" class="h-100">
                    <div class="text-center mt-3 mb-2">
                        <label class="small text-muted m-0">Scored:</label><br>
                        <span class="m-0">{{ quizTaken.correctAnswers }} / {{ quizTaken.questionsLength }}</span><br>
                        <label class="small text-muted m-0">XP gained:</label><br>
                        <span class="m-0">{{ quizTaken.score }}</span>
                    </div>
                </div>
            
                <div class="float-left ml-1 pt-1">
                    <i class="align-text-bottom material-icons">thumb_up</i>
                    <span>{{ quiz.upvotes }}</span>
                </div>
                
                <div class="float-right mr-1 pt-1">
                    <span>{{ quiz.views }}</span>
                    <i class="align-text-bottom material-icons">visibility</i>
                </div>
            </div>
        </div>
        <div class="col">
            <div class="px-2 text-center">
                <h6 class="m-0 overflow-hidden">{{ quiz.topic }}</h6>
                <p class="m-0 lead small text-muted">{{ quiz.creationDateFormatted }}</p>
                <span class="w-100 badge badge-info">{{ quiz.category }}</span><br>
                <a class="w-100 badge badge-light" :href="'/user/' + quiz.ownerUsername">{{ quiz.ownerUsername }}</a><br>
                <span v-if="quiz.difficulty == 'easy'" class="w-100 badge badge-success">{{ quiz.difficulty }}</span>
                <span v-else-if="quiz.difficulty == 'medium'" class="w-100 badge badge-warning">{{ quiz.difficulty }}</span>
                <span v-else-if="quiz.difficulty == 'hard'" class="w-100 badge badge-danger">{{ quiz.difficulty }}</span>
            </div>
            <div class="w-100 position-absolute" style="bottom: 0;">
                <div class="w-100 btn-group btn-group-sm" role="group" aria-label="...">
                    <a class="btn btn-primary" :href="'/quiz/' + quiz.id">Play</a>
                    <a v-if="canEdit" class="btn btn-light" :href="'/quiz/edit/' + quiz.id">Edit</a>
                    <a v-if="showSummary" class="btn btn-light" :href="'/quiz/summary/' + quiz.id">Summary</a>
                </div>
            </div>
        </div>
    </div>
</div>
`
});