<template>
  <div class="circle">
    <div><span class="block"></span><span>404</span><span class="block"></span></div>
  </div>
</template>

<script>
export default {
  name: 'Page404',
  computed: {
    message() {
      return 'The webmaster said that you can not enter this page...'
    },
  },
}
</script>

 <style>
.circle {
  width: 200px;
  height: 200px;
  border-radius: 200px;
  border: 15px solid #b22727;
}

.circle > div {
  color: #b22727;
  font: bolder 50px arial;
  transform: matrix(0.642788, -0.766044, 0.766044, 0.642788, 0, 95);
  -ms-transform: matrix(0.642788, -0.766044, 0.766044, 0.642788, 0, 95);
  -moz-transform: matrix(0.642788, -0.766044, 0.766044, 0.642788, 0, 95);
  -o-transform: matrix(0.642788, -0.766044, 0.766044, 0.642788, 0, 95);
  position: absolute;
  top: 0;
  left: 0;
}

.block {
  width: 60px;
  display: inline-block;
  height: 15px;
  background-color: #b22727;
  margin: 12px 10px;
}
</style>