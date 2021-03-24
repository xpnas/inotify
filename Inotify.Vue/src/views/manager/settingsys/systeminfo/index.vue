<template>
<div class="app-container">
    <div class="block" align="center">
        <el-date-picker :default-time="['00:00:00', '23:59:59']" @change="change" v-model="dateValue" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期"> </el-date-picker>
    </div>
    <div align="center">
        <div id="SendTypeInfo" style="width: 80%; height: 350px"></div>
        <div id="SendInfo" style="width: 80%; height: 400px" align="center"></div>
    </div>
</div>
</template>

<script>
import moment from 'moment'
import echarts from 'echarts'
import {
    getSendTypeInfos,
    getSendInfos
} from '@/api/systemsetting'
export default {
    filters: {
        statusFilter(status) {
            const statusMap = {
                published: 'success',
                draft: 'gray',
                deleted: 'danger',
            }
            return statusMap[status]
        },
    },
    data() {
        return {
            SendTypeInfoChart: '',
            SendTypeInfoChartData: [{
                value: 1,
                name: '全部',
            }, ],
            SendInfoChart: '',
            listLoading: false,
            dateValue: '',
        }
    },
    created() {
        this.fetchData()
    },
    mounted() {
        const start = new Date()
        const end = new Date()
        start.setTime(end.getTime() - 3600 * 1000 * 24 * 30)
        this.dateValue = [start, end]
        this.change()
    },
    methods: {
        fetchData() {},
        change() {
            const startDate = new moment(this.dateValue[0]).format('YYYYMMDD')
            const endData = new moment(this.dateValue[1]).format('YYYYMMDD')
            this.drawSendInfo(startDate, endData)
        },
        drawSendInfo(start, end) {
            this.listLoading = true;
            this.SendInfoChart = echarts.init(document.getElementById('SendInfo'))
            this.SendTypeInfoChart = echarts.init(document.getElementById('SendTypeInfo'))
            getSendInfos(start, end).then((response) => {
                this.SendTypeInfoChart.setOption({
                    roseType: 'angle',
                    tooltip: {},
                    series: [{
                        name: '消息类型',
                        type: 'pie',
                        radius: '55%',
                        data: response.data.items,
                    }, ],
                })

                this.SendInfoChart.setOption({
                    title: {
                        text: '消息统计',
                    },
                    tooltip: {},
                    xAxis: {
                        type: 'category',
                        data: response.data.dataX,
                    },
                    yAxis: {
                        type: 'value',
                    },
                    series: [{
                        data: response.data.dataY,
                        type: 'line',
                    }, ]
                })
                this.listLoading = false;
            })
        }
    },
}
</script>
