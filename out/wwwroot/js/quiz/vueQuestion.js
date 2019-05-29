Vue.component('question', {
    props: ['type', 'text', 'alternatives', 'imagePath', 'timer', 'questionsLength', 'xpGained', 'isAuthenticated', 'lastQuestion'],
    template:
`
<div class="text-center">
    <div style="position: relative; height: 250px; max-height: 250px;">
        <div class="display-4 w-100 center-vertical">{{ text }}</div>
    </div>
    <div class="w-100" v-if="imagePath !== null">
        <hr>
        <img class="d-block m-auto" style="width: 500px;" :src="imagePath"/>
    </div>
    <hr>
    <label class="lead small text-muted m-0">Experience Points</label>
    <p v-if="isAuthenticated" class="lead w-100">{{ xpGained }}</p>
    <p v-else class="lead small text-muted w-100">Login to gain experience</p>
    <div class="progress" id="progressBars">
        <div v-for="i in questionsLength" class="progress-bar-info border lead small text-muted" role="progressbar" style="width: 100%">{{ i }}</div>
    </div>
    <hr>
    <div id="question">
        <div v-if="type === 'multiple_choice'" class="form-row w-100">
            <div class="col">
                <button v-on:click="$emit('answer', $event)" type="button" name="A" class="btn btn-dark m-1 w-100 border">{{ alternatives[0] }}</button>
                <button v-on:click="$emit('answer', $event)" type="button" name="B" class="btn btn-dark m-1 w-100 border">{{ alternatives[1] }}</button>
            </div>
            <div class="col">
                <button v-on:click="$emit('answer', $event)" type="button" name="C" class="btn btn-dark m-1 w-100 border">{{ alternatives[2] }}</button>
                <button v-on:click="$emit('answer', $event)" type="button" name="D" class="btn btn-dark m-1 w-100 border">{{ alternatives[3] }}</button>
            </div>
        </div>
        <div v-else-if="type === 'true_false'" class="form-row w-100">
            <div class="col">
                <button v-on:click="$emit('answer', $event)" type="button" name="T" class="btn btn-dark m-1 w-100 border">True</button>
            </div>
            <div class="col">
                <button v-on:click="$emit('answer', $event)" type="button" name="F" class="btn btn-dark m-1 w-100 border">False</button>
            </div>
        </div>   
    </div>
    <hr>
    
    <div class="progress position-relative">
        <div id="progressTimer" class="progress-bar progress-bar-striped progress-bar-animated" role-="progressbar" style="width: 100%"></div>
        <p class="justify-content-center d-flex position-absolute w-100">{{ timer }} seconds left</p>
    </div>
    
    <div id="nextBtnCollapse" class="collapse">
        <button v-if="lastQuestion" v-on:click="$emit('nextstate', 'summary')" class="lead btn btn-primary w-100 mt-2" >Show Summary</button>
        <button v-else v-on:click="$emit('next', $event)" type="button" class="lead btn btn-primary w-100 mt-2">Next</button>
    </div>
</div>
`
});

Vue.component('intro', {
    props: ['id', 'topic', 'description', 'category', 'imagePath', 'difficulty', 'questionsLength'],
    template:
`
<div class="text-center">
    <div v-if="imagePath !== null">
        <img style="display:block; margin:auto; max-width: 400px;" :src="imagePath"/>
    </div>
    
    <label class="text-muted mb-0">Title</label>
    <h1 class="display-5">{{ topic }}</h1>
    
    <label class="text-muted">Description</label>
    <p class="lead">{{ description }}</p>
    
    <label class="text-muted">Category</label>
    <p class="lead">{{ category }}</p>
    
    <label class="text-muted">Difficulty</label>
    <p class="lead">
        <span v-if="difficulty == 'easy'" class="badge badge-pill badge-success">{{ difficulty }}</span>
        <span v-else-if="difficulty == 'medium'" class="badge badge-pill badge-warning">{{ difficulty }}</span>
        <span v-else-if="difficulty == 'hard'" class="badge badge-pill badge-danger">{{ difficulty }}</span>
    </p>
    
    <label class="text-muted">Questions</label>
    <p class="lead">{{ questionsLength }}</p>
    <a :href="'/leaderboard/' + id">Leaderboards</a>
    <hr class="my-4">
    
    <p class="lead">
        <button v-on:click="$emit('nextstate', 'question')" class="lead btn btn-primary w-100">Start Quiz</button>
    </p>
</div>
`
});