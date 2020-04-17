import { DateTime } from '@/utils/common'

export default class OrderPayment {
  id: number = 0

  amount: number = 0

  order_id: number = 0

  index: number = 0

  comment: string = ''

  is_paid: boolean = false

  due_time: string = DateTime.minValue

  paid_at: string = DateTime.minValue

}
