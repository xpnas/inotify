import Vue from 'vue'
import Router from 'vue-router'

Vue.use(Router)

/* Layout */
import Layout from '@/layout'

/**
 * Note: sub-menu only appear when route children.length >= 1
 * Detail see: https://panjiachen.github.io/vue-element-admin-site/guide/essentials/router-and-nav.html
 *
 * hidden: true                   if set true, item will not show in the sidebar(default is false)
 * alwaysShow: true               if set true, will always show the root menu
 *                                if not set alwaysShow, when item has more than one children route,
 *                                it will becomes nested mode, otherwise not show the root menu
 * redirect: noRedirect           if set noRedirect will no redirect in the breadcrumb
 * name:'router-name'             the name is used by <keep-alive> (must set!!!)
 * meta : {
    roles: ['admin','editor']    control the page roles (you can set multiple roles)
    title: 'title'               the name show in sidebar and breadcrumb (recommend set)
    icon: 'svg-name'/'el-icon-x' the icon show in the sidebar
    breadcrumb: false            if set false, the item will hidden in breadcrumb(default is true)
    activeMenu: '/example/list'  if set path, the sidebar will highlight the path you set
  }
 */

/**
 * constantRoutes
 * a base page that does not have permission requirements
 * all roles can be accessed
 */
export const constantRoutes = [
  {
    path: '/login',
    component: () => import('@/views/login/index'),
    hidden: true,
    props: true
  },
  {
    path: '/404',
    component: () => import('@/views/404'),
    hidden: true
  },
  {
    path: '/',
    component: Layout,
    redirect: '/manager',
    children: [
      {
        path: 'manager',
        name: 'Manager',
        component: () => import('@/views/manager/settingpro/sendkey/index'),
        meta: { roles: ['user', 'system'], title: '后台管理', icon: 'el-icon-s-home' }
      }
    ]
  },
  {
    path: '/settingpro',
    component: Layout,
    redirect: '/settingpro/sendmethods',
    name: 'settingpro',
    meta: { roles: ['user', 'system'], title: '个人设置', icon: 'el-icon-user' },
    children: [
      {
        path: 'sendkey',
        name: 'sendkey',
        component: () => import('@/views/manager/settingpro/sendkey/index'),
        meta: { roles: ['user', 'system'], title: '消息验证', icon: 'el-icon-s-comment' }
      },
      {
        path: 'sendmethods',
        name: 'sendmethods',
        component: () => import('@/views/manager/settingpro/sendauths/index'),
        meta: { roles: ['user', 'system'], title: '消息通道', icon: 'el-icon-s-promotion' }
      },
      {
        path: 'oauthsetting',
        name: 'oauthsetting',
        component: () => import('@/views/manager/settingpro/oauthsetting/index'),
        meta: { roles: ['user', 'system'], title: '重置密码', icon: 'el-icon-s-custom' }
      }
    ]
  },
  {
    path: '/settingsys',
    component: Layout,
    redirect: '/settingsys/systeminfo',
    name: 'settingsys',
    meta: { roles: ['system'], title: '系统设置', icon: 'el-icon-setting' },
    children: [
      {
        path: 'systeminfo',
        name: 'systeminfo',
        component: () => import('@/views/manager/settingsys/systeminfo/index'),
        meta: { roles: ['system'], title: '系统状态', icon: 'el-icon-s-data' }
      },
      {
        path: 'systemusers',
        name: 'systemusers',
        component: () => import('@/views/manager/settingsys/usermanager/index'),
        meta: { roles: ['system'], title: '用户管理', icon: 'el-icon-s-check' }
      },
      {
        path: 'systemjwt',
        name: 'systemjwt',
        component: () => import('@/views/manager/settingsys/jwt/index'),
        meta: { roles: ['system'], title: 'JWT验证', icon: 'el-icon-s-platform' }
      },
      {
        path: 'systemglobal',
        name: 'systemglobal',
        component: () => import('@/views/manager/settingsys/systemglobal/index'),
        meta: { roles: ['system'], title: '全局参数', icon: 'el-icon-s-tools' }
      }
    ]
  },
  {
    path: 'external-link',
    component: Layout,
    children: [
      {
        path: 'https://github.com/xpnas/Inotify',
        meta: { title: '源码', icon: 'el-icon-share' }
      }
    ]
  },

  // 404 page must be placed at the end !!!
  { path: '*', redirect: '/404', hidden: true }
]

const createRouter = () =>
  new Router({
    // mode: 'history', // require service support
    scrollBehavior: () => ({ y: 0 }),
    routes: constantRoutes
  })

const router = createRouter()

// Detail see: https://github.com/vuejs/vue-router/issues/1234#issuecomment-357941465
export function resetRouter() {
  const newRouter = createRouter()
  router.matcher = newRouter.matcher // reset router
  router.push({
    name: '/login',
    params: {
      token: ''
    }
  })
}

export default router
